import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { textSummary } from './libs/k6-jslib-summary-0.1.0/index.js';
import { htmlReport } from './libs/k6-reporter.2-4-0.bundle.js';
import Big from './libs/big.7-0-1.mjs';
import { generateEntryControlPayload, getFormattedDateOnly, getRandomTime, parameters } from './utils.js';

const baseDate = new Date(2024, 3, 27);
const dateOnlyStr = getFormattedDateOnly(baseDate);
const reportUrl = `${parameters.baseUrl}/api/v1/DailyConsolidatedReport/${dateOnlyStr}`;

const numberOfEntryControlsPerCycle = 10;
const delayBetweenEntriesMs = 100;
const delayBeforeReportMs = 2000;

export const options = {
    vus: 1,
    iterations: 5,
};

export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: '  ', enableColors: true }),
        './results/summary-logic.json': JSON.stringify(data),
        './results/summary-logic.html': htmlReport(data),
    };
}

export function setup() {
    console.log(`SETUP: Verifying initial zero state for date: ${dateOnlyStr}`);
    const reportUrl = `${parameters.baseUrl}/api/v1/DailyConsolidatedReport/${dateOnlyStr}`;
    const reportRes = http.get(reportUrl, { tags: { name: 'Setup_FetchInitialReport' } });

    const isSuccess = check(reportRes, {
        'SETUP: Initial report fetch successful (200)': (r) => r.status === 200,
    });
    
    if (!isSuccess) {
        console.error(`SETUP: Failed to fetch initial report for ${dateOnlyStr}: Status ${reportRes.status}. Body: ${reportRes.body}`);
        throwError(`Request to get initial state for date ${dateOnlyStr} returned failure.`);
    }

    const reportData = reportRes.json();
    const initialCredits = new Big(reportData.totalCredits);
    const initialDebits = new Big(reportData.totalDebits);
    
    if (!initialCredits.eq(0) || !initialDebits.eq(0)) {
        console.error(`SETUP: Initial state for ${dateOnlyStr} is NOT ZERO. Credits: ${initialCredits.toFixed(2)}, Debits: ${initialDebits.toFixed(2)}`);
        throwError(`Initial state for date ${dateOnlyStr} is not zero.`);
    }

    console.log(`SETUP: Initial state for ${dateOnlyStr} is ZERO. Credits: ${initialCredits.toFixed(2)}, Debits: ${initialDebits.toFixed(2)}`);
    
    console.log('--');
    console.log('--');

    return {};

    function throwError(message) {
        throw new Error(message + ' Aborting test.');
    }
}

export default function () {
    console.log(`Starting iteration ${__ITER} for date: ${dateOnlyStr}. VU: ${__VU}`);

    let initialValues;
    group('1. Fetch Current Report State for Iteration', () => 
        initialValues = fetchCurrentReportState());

    console.log('--');
    console.log('--');

    let sentTransactionsDataSentInThisIteration;
    group('2. Send EntryControl Transactions', () =>
        sentTransactionsDataSentInThisIteration = sendEntryControlReleases());

    const expectedTotalTransactions = {
        credits: initialValues.credits.plus(sentTransactionsDataSentInThisIteration.credits),
        debits: initialValues.debits.plus(sentTransactionsDataSentInThisIteration.debits),
    };


    console.log(`Iter ${__ITER}: Finished sending entries.`);
    console.log(`  Credits existing prior to this iteration: ${initialValues.credits.toFixed(2)}`);
    console.log(`  Debits existing prior to this iteration: ${initialValues.debits.toFixed(2)}`);
    console.log(`  Credits Added in This Iteration: ${sentTransactionsDataSentInThisIteration.credits.toFixed(2)}`);
    console.log(`  Debits Added in This Iteration: ${sentTransactionsDataSentInThisIteration.debits.toFixed(2)}`);
    console.log(`  Expected Total Credits After This Iteration: ${expectedTotalTransactions.credits.toFixed(2)}`);
    console.log(`  Expected Total Debits After This Iteration: ${expectedTotalTransactions.debits.toFixed(2)}`);

    if (delayBeforeReportMs > 0) {
        console.log(`Iter ${__ITER}: Waiting ${delayBeforeReportMs}ms for report consolidation...`);
        sleep(delayBeforeReportMs / 1000);
    }

    console.log('--');
    console.log('--');
    
    group('3. Verify DailyConsolidatedReport After Entries', () =>
        verifyDailyConsolidatedReport(initialValues, sentTransactionsDataSentInThisIteration, expectedTotalTransactions)
    );

    console.log(`Completed iteration ${__ITER} for date: ${dateOnlyStr}. VU: ${__VU}`);

    console.log('----');
    console.log('----');
}

function fetchCurrentReportState() {
    const reportRes = http.get(reportUrl, { tags: { name: 'FetchCurrentReportState_Iter' } });

    let isSuccess = check(reportRes, {
        [`Iter ${__ITER}: Current report GET status is 200`]: (r) => r.status === 200,
    });

    if (!isSuccess) {
        console.error(`Iter ${__ITER}: Failed to reliably fetch current report state. Status: ${reportRes.status}. Body: ${reportRes.body}.`);
        check(null, {
            'Baseline_Report_Fetch_Failed_Impacts_Sum_Checks': () => false,
        }, { iter_aborted_due_to_fetch_fail: true, iteration: __ITER });
        
        throw new Error(`Iter ${__ITER}: Aborting iteration due to report fetch failure.`);
    }

    const reportData = reportRes.json();
    const result = {
        credits: new Big(reportData.totalCredits),
        debits: new Big(reportData.totalDebits),
    };
    
    console.log(`Iter ${__ITER}: State before new entries - Credits: ${result.credits.toFixed(2)}, Debits: ${result.debits.toFixed(2)}`);
    return result;
}

function sendEntryControlReleases() {

    let totalCreditsSent = new Big(0);
    let totalDebitsSent = new Big(0);

    for (let i = 0; i < numberOfEntryControlsPerCycle; i++) {
        const payloadObj = sendEntryControlRelease(i);

        switch (payloadObj.type) {
            case 'C':
                totalCreditsSent = totalCreditsSent.plus(payloadObj.value);
                break;
            case 'D':
                totalDebitsSent = totalDebitsSent.plus(payloadObj.value);
                break;
        }
        
        if (delayBetweenEntriesMs > 0)
            sleep(delayBetweenEntriesMs / 1000);
    }

    return { credits: totalCreditsSent, debits: totalDebitsSent };
}

function sendEntryControlRelease(releaseIndex) {
    const timeStr = getRandomTime();
    const dateTimeStr = `${dateOnlyStr}T${timeStr}`;
    const entryControlUrl = `${parameters.baseUrl}/api/v1/EntryControl/${dateTimeStr}`;

    const payloadObj = generateEntryControlPayload();
    const payloadStr = JSON.stringify(payloadObj);

    const postParams = {
        headers: { 'Content-Type': 'application/json' },
        tags: { name: 'EntryControl-POST_Iter' }
    };

    const res = http.post(entryControlUrl, payloadStr, postParams);

    const checkTag = `Iter ${__ITER}: EntryControl POST ${releaseIndex + 1} status is 201`;
    const isSuccess = check(res, { [checkTag]: (r) => r.status === 201 });
    
    if (!isSuccess) {
        console.error(`Iter ${__ITER}: EntryControl POST ${releaseIndex + 1} failed: Status ${res.status} - Body: ${res.body}`);
        check(null, {
            [`${checkTag}_Failed`]: () => false,
        }, { error_reason: "EntryControl_POST_Failed", iteration: __ITER });
        return;
    }

    return payloadObj;
}

function verifyDailyConsolidatedReport(initialValues, sentTransactionsDataSentInThisIteration, expectedTotalTransactions) {
    const reportUrl = `${parameters.baseUrl}/api/v1/DailyConsolidatedReport/${dateOnlyStr}`;
    const finalReportRes = http.get(reportUrl, { tags: { name: 'VerifyFinalReport_Iter' } });

    const isSuccess = check(finalReportRes, {
        [`Iter ${__ITER}: Final DailyReport GET status is 200`]: (r) => r.status === 200,
    }, { date_tested: dateOnlyStr, iteration: __ITER });

    if (!isSuccess) {
        console.error(`Iter ${__ITER}: Final DailyReport GET for ${dateOnlyStr} failed: Status ${finalReportRes.status} - Body: ${finalReportRes.body}`);
        check(null, {
            'TotalCredits match expected sum': () => false,
            'TotalDebits match expected sum': () => false,
        }, { date_tested: dateOnlyStr, error_reason: "Final_Report_Fetch_Failed", iteration: __ITER });
        
        return;
    }

    const reportData = finalReportRes.json();
    const reportTotal = {
        credits: new Big(reportData.totalCredits),
        debits: new Big(reportData.totalDebits),
    }

    console.log(`Iter ${__ITER}: Final Report - API Credits: ${reportTotal.credits.toFixed(2)}, API Debits: ${reportTotal.debits.toFixed(2)}`);

    const checks = {
        'TotalCredits match expected sum': () => reportTotal.credits.eq(expectedTotalTransactions.credits),
        'TotalDebits match expected sum': () => reportTotal.debits.eq(expectedTotalTransactions.debits),
    };
    const allSumsMatch = check(finalReportRes, checks, { date_tested: dateOnlyStr, iteration: __ITER });

    if (!allSumsMatch) {
        if (!reportTotal.credits.eq(expectedTotalTransactions.credits)) {
            console.error(`Iter ${__ITER}: MISMATCH for Credits on ${dateOnlyStr}: Expected ${expectedTotalTransactions.credits.toFixed(2)}, but got ${reportTotal.credits.toFixed(2)}.`);
            console.error(`  (Base for iter: ${initialValues.credits.toFixed(2)}, Added this iter: ${sentTransactionsDataSentInThisIteration.credits.toFixed(2)})`);
        }
        if (!reportTotal.debits.eq(expectedTotalTransactions.debits)) {
            console.error(`Iter ${__ITER}: MISMATCH for Debits on ${dateOnlyStr}: Expected ${expectedTotalTransactions.debits.toFixed(2)}, but got ${reportTotal.debits.toFixed(2)}.`);
            console.error(`  (Base for iter: ${initialValues.debits.toFixed(2)}, Added this iter: ${sentTransactionsDataSentInThisIteration.debits.toFixed(2)})`);
        }
    }
}