import ws from 'k6/ws';
import { check, sleep } from 'k6';

const HUB_URL = __ENV.HUB_URL || 'ws://localhost:5287/hubs/file-processing';
const TOKEN = __ENV.TOKEN || 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJEb2N1Q2hhdC5DbGllbnQiLCJpc3MiOiJEb2N1Q2hhdC5BcGkiLCJleHAiOjE3ODgzNjc4MzQsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMDE5Zjg0ZjItMWNhYS03MmU1LThhZmUtZDQ1YjFkOTlmYjU0IiwiZW1haWwiOiJ0ZXN0QGdtYWlsLmNvbSIsImp0aSI6ImMzODRiNWZhLTU2ZDQtNDA3Zi04MTBmLWMxMzE0N2E3YzdmOSIsImlhdCI6MTc4ODI4MTQzNCwibmJmIjoxNzg4MjgxNDM0fQ.v2QQ0NMJ4TAA1Zw7VBWGwsoYcjSETXigmWnSJSDoJTs';
const FILE_IDS = (__ENV.FILE_IDS || '019f84f2-1caa-72e5-8afe-d45b1d99fb54')
    .split(',')
    .map((id) => id.trim())
    .filter(Boolean);

const recordSeparator = String.fromCharCode(0x1e);

export const options = {
    stages: [
        { duration: '20s', target: 10 },
        { duration: '40s', target: 50 },
        { duration: '20s', target: 0 },
    ],
    thresholds: {
        checks: ['rate>0.95'],
    },
};

export default function () {
    const url = TOKEN
        ? `${HUB_URL}?access_token=${encodeURIComponent(TOKEN)}`
        : HUB_URL;

    const response = ws.connect(url, {}, (socket) => {
        let handshakeCompleted = false;
        let watchSent = false;
        let unwatchSent = false;

        socket.on('open', () => {
            socket.send(JSON.stringify({ protocol: 'json', version: 1 }) + recordSeparator);
        });

        socket.on('message', (message) => {
            const frames = message
                .split(recordSeparator)
                .map((frame) => frame.trim())
                .filter(Boolean);

            if (!handshakeCompleted) {
                handshakeCompleted = true;
                watchSent = true;
                socket.send(
                    JSON.stringify({
                        type: 1,
                        invocationId: `${__VU}-${__ITER}-watch`,
                        target: 'WatchFiles',
                        arguments: [FILE_IDS],
                    }) + recordSeparator
                );
            }

            for (const frame of frames) {
                const parsed = JSON.parse(frame);

                if (parsed.type === 3 && parsed.invocationId === `${__VU}-${__ITER}-watch`) {
                    check(parsed, {
                        'WatchFiles completed': (value) => !value.error,
                    });
                }

                if (parsed.type === 3 && parsed.invocationId === `${__VU}-${__ITER}-unwatch`) {
                    check(parsed, {
                        'UnwatchFiles completed': (value) => !value.error,
                    });
                    socket.close();
                }
            }
        });

        socket.setTimeout(() => {
            if (watchSent && !unwatchSent) {
                unwatchSent = true;
                socket.send(
                    JSON.stringify({
                        type: 1,
                        invocationId: `${__VU}-${__ITER}-unwatch`,
                        target: 'UnwatchFiles',
                        arguments: [FILE_IDS],
                    }) + recordSeparator
                );
            }
        }, 1000);

        socket.setTimeout(() => {
            socket.close();
        }, 5000);
    });

    check(response, {
        'websocket status is 101': (r) => r && r.status === 101,
    });

    sleep(1);
}
