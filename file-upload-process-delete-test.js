import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5287/api';
const ORG_ID = __ENV.ORG_ID || '';
const PROCESS_WAIT_SECONDS = Number(__ENV.PROCESS_WAIT_SECONDS || '2');

export const options = {
    stages: [
        { duration: '20s', target: 5 },
        { duration: '40s', target: 15 },
        { duration: '20s', target: 0 },
    ],
};

function parseFileId(response) {
    let body;

    try {
        body = response.json();
    } catch {
        return null;
    }

    return body?.id || body?.Id;
}

export default function () {
    const fileName = `k6-file-${__VU}-${__ITER}-${Date.now()}.txt`;
    const content = `
    k6 upload/process/delete stress test file.
    Virtual user: ${__VU}
    Iteration: ${__ITER}
    Created at: ${new Date().toISOString()}
    This text is intentionally repeated so the ingestion pipeline has content to extract and chunk.
    This text is intentionally repeated so the ingestion pipeline has content to extract and chunk.
  `;

    const headers = ORG_ID ? { 'x-org-id': ORG_ID } : {};

    const uploadResponse = http.post(
        `${BASE_URL}/files/upload`,
        {
            file: http.file(content, fileName, 'text/plain'),
        },
        { headers },
    );

    const uploadOk = check(uploadResponse, {
        'upload status is 201': (r) => r.status === 201,
        'upload returned file id': (r) => Boolean(parseFileId(r)),
    });

    if (!uploadOk) {
        return;
    }

    const fileId = parseFileId(uploadResponse);

    const processResponse = http.post(`${BASE_URL}/files/${fileId}/process`);

    check(processResponse, {
        'process status is 200': (r) => r.status === 200,
    });

    sleep(PROCESS_WAIT_SECONDS);

    const deleteResponse = http.del(`${BASE_URL}/files/${fileId}`);

    check(deleteResponse, {
        'delete status is 204 or 404': (r) => r.status === 204 || r.status === 404,
    });

    sleep(1);
}