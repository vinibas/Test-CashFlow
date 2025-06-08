import http from 'k6/http';
import { check, sleep } from 'k6';
import { textSummary } from 'k6/metrics';
import { htmlReport } from './libs/k6-reporter.2-4-0.bundle.js';
import { randomIntBetween } from './libs/k6-utils-1.4.0/index.js';

export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        './results/summary.json': JSON.stringify(data),
        "./results/summary.html": htmlReport(data),
    };
}

const baseUrl = __ENV.K6_BASE_URL || 'https://localhost:7027';
const baseDate = new Date(2024, 6, 27);

function getFormattedDateOnly(dateObj) {
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function getRandomTime() {
    const hours = String(randomIntBetween(0, 23)).padStart(2, '0');
    const minutes = String(randomIntBetween(0, 59)).padStart(2, '0');
    const seconds = String(randomIntBetween(0, 59)).padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
}

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
        'http_req_duration{scenario:entry_control_scenario}': ['p(95)<1000'],
        'http_req_failed{scenario:daily_report_scenario}': ['rate<0.05'],
        'http_req_duration{scenario:daily_report_scenario}': ['p(95)<1000'],
    },
};

export function entryControlTest() {
    const dateOnlyStr = getFormattedDateOnly(baseDate);
    const timeStr = getRandomTime();
    // Expected RoundtripKind format: YYYY-MM-DDTHH:mm:ss
    const dateTimeStr = `${dateOnlyStr}T${timeStr}`;
    const url = `${baseUrl}/api/v1/EntryControl/${dateTimeStr}`;
    const payload = generateEntryControlPayload();
    const params = { headers: { 'Content-Type': 'application/json' } };

    const res = http.post(url, payload, params);

    check(res, { 'EntryControl POST status is 201': (r) => r.status === 201 });

    sleep(0.1);
}

function generateEntryControlPayload() {
    const randomValue = parseFloat((randomIntBetween(1, 100000) / 100).toFixed(2));
    const randomType = randomIntBetween(0, 1) === 0 ? 'C' : 'D';

    return JSON.stringify({
        value: randomValue,
        type: randomType,
        description: 'Test executed by K6 (file script-load.js).'
    });
}

export function dailyReportTest() {
    const dateOnlyStr = getFormattedDateOnly(baseDate);
    const url = `${baseUrl}/api/v1/DailyConsolidatedReport/${dateOnlyStr}`;

    const res = http.get(url);

    check(res, { 'DailyReport GET status is 200': (r) => r.status === 200 });

    sleep(0.1);
}

