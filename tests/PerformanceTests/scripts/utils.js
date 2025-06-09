import { randomIntBetween } from './libs/k6-utils-1.4.0/index.js';

export const parameters = {
    baseUrl: __ENV.K6_BASE_URL || 'https://localhost:7027',
};

export function getFormattedDateOnly(dateObj) {
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

export function getRandomTime() {
    const hours = String(randomIntBetween(0, 23)).padStart(2, '0');
    const minutes = String(randomIntBetween(0, 59)).padStart(2, '0');
    const seconds = String(randomIntBetween(0, 59)).padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
}

export function generateEntryControlPayload() {
    const randomValue = parseFloat((randomIntBetween(1, 100000) / 100).toFixed(2));
    const randomType = randomIntBetween(0, 1) === 0 ? 'C' : 'D';

    return {
        value: randomValue,
        type: randomType,
        description: 'Test executed by K6 (file script-load.js).'
    };
}
export function generateEntryControlPayloadStringfy() {
    return JSON.stringify(generateEntryControlPayload());
}
