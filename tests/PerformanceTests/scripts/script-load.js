import http from 'k6/http';
import { check, sleep } from 'k6';
import { textSummary } from './libs/k6-jslib-summary-0.1.0/index.js';
import { htmlReport } from './libs/k6-reporter.2-4-0.bundle.js';
import { generateEntryControlPayloadStringfy, getFormattedDateOnly, getRandomTime, parameters } from './utils.js';

const baseDate = new Date(2024, 6, 27);
const dateOnlyStr = getFormattedDateOnly(baseDate);
const params = { headers: { 'Content-Type': 'application/json' } };

export const options = {
    scenarios: {
        entry_control_scenario: {
            executor: 'constant-arrival-rate',
            rate: 50, // 50 requests per second
            timeUnit: '1s',
            duration: '5m',
            preAllocatedVUs: 50,
            maxVUs: 150,
            exec: 'entryControlTest',
        },
        daily_report_scenario: {
            executor: 'constant-arrival-rate',
            rate: 50, // 50 requests per second
            timeUnit: '1s',
            duration: '5m',
            preAllocatedVUs: 50,
            maxVUs: 150,
            exec: 'dailyReportTest',
        },
    },
    thresholds: {
        'http_req_failed{scenario:entry_control_scenario}': ['rate<0.05'],
        'http_req_duration{scenario:entry_control_scenario}': ['p(95)<1500'],
        'http_req_failed{scenario:daily_report_scenario}': ['rate<0.05'],
        'http_req_duration{scenario:daily_report_scenario}': ['p(95)<1500'],
    },
};

export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        './results/summary-load.json': JSON.stringify(data),
        "./results/summary-load.html": htmlReport(data),
    };
}

export function entryControlTest() {
    const timeStr = getRandomTime();
    // Expected RoundtripKind format: YYYY-MM-DDTHH:mm:ss
    const dateTimeStr = `${dateOnlyStr}T${timeStr}`;
    const url = `${parameters.baseUrl}/api/v1/EntryControl/${dateTimeStr}`;
    const payload = generateEntryControlPayloadStringfy();

    const res = http.post(url, payload, { ...params, tags: { name: 'EntryControl' } });

    const checkResult = check(res, { 'EntryControl POST status is 201': (r) => r.status === 201 });

    if (!checkResult)
        console.log(`EntryControl POST failed: Status=${res.status}, URL=${url}, Payload=${payload}, Response=${res.body}`);

    sleep(1);
}

export function dailyReportTest() {
    const url = `${parameters.baseUrl}/api/v1/DailyConsolidatedReport/Extended/${dateOnlyStr}`;

    const res = http.get(url, { tags: { name: 'DailyReport' } });

    const checkResult = check(res, { 'DailyReport GET status is 200': (r) => r.status === 200 });

    if (!checkResult)
        console.log(`DailyConsolidatedReport GET failed: Status=${res.status}, URL=${url}, Payload=${payload}, Response=${res.body}`);

    sleep(1);
}

