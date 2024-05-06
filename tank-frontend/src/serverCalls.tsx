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

type TankInfo = {
    readonly magazine: string;
    readonly position: { x: number; y: number };
    readonly orientation: number;
    readonly moving: boolean;
}

export type PlayerInfoMessage = {
    readonly name: string;
    readonly scores: Scores;
    readonly controls: string;
    readonly tank?: TankInfo;
    readonly openConnections: number;
}

export type MapInfo = {
    readonly name: string;
    readonly typeName: string;
    readonly preview: string;
}

export function useMyWebSocket<T = unknown>(url: string, options: Options = {}) {
    return useWebSocket<T>(url, {
        shouldReconnect: () => true,
        reconnectAttempts: 2,
        onReconnectStop: () => alert('server connection failed. please reload.'),
        ...options
    });
}
