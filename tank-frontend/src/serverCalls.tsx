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

type Tank = {
    readonly pixelPosition: { x: number; y: number };
    readonly orientation: number;
    readonly moving: boolean;
    readonly bulletStats: BulletStats;
    readonly reloadingUntil: string;
    readonly nextShotAfter: string;
    readonly usedBullets: number;
    readonly maxBullets: number;
}

export type Player = {
    readonly name: string;
    readonly scores: Scores;
    readonly openConnections: number;
    readonly lastInput: string;
}

export type PlayerInfoMessage = {
    readonly player: Player;
    readonly controls: string;
    readonly tank?: Tank;
}

export type MapInfo = {
    readonly name: string;
    readonly typeName: string;
    readonly preview: string;
}

export type BulletStats = {
    speed: number;
    acceleration: number,
    explosive: boolean,
    smart: boolean
};

export function useMyWebSocket<T = unknown>(url: string, options: Options = {}) {
    return useWebSocket<T>(url, {
        shouldReconnect: () => true,
        reconnectAttempts: 2,
        onReconnectStop: () => alert('server connection failed. please reload.'),
        ...options
    });
}
