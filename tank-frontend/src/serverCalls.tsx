import useWebSocket, {Options} from 'react-use-websocket';

export function makeApiUrl(path: string, protocol: 'http' | 'ws' = 'http') {
    return new URL(`${protocol}://${window.location.hostname}${path}`);
}

export type Scores = {
    readonly kills: number;
    readonly deaths: number;
    readonly wallsDestroyed: number;
    readonly shotsFired: number;
    readonly overallScore: number;
    readonly powerUpsCollected: number;
    readonly pixelsMoved: number;
};

export type Player = {
    readonly name: string;
    readonly scores: Scores;
};

export function useMyWebSocket<T = unknown>(url: string, options: Options) {
    return useWebSocket<T>(url, {
        shouldReconnect: () => true,
        reconnectAttempts: 5,
        onReconnectStop: () => alert('server connection failed. please reload.'),
        ...options
    });
}
