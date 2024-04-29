export function makeApiUrl(path: string, protocol: 'http' | 'ws' = 'http') {
    return new URL(`${protocol}://${window.location.hostname}${path}`);
}

export type Scores = {
    readonly kills: number;
    readonly deaths: number;
    readonly wallsDestroyed: number;
    readonly shotsFired: number;
    readonly overallScore: number;
};

export type Player = {
    readonly name: string;
    readonly scores: Scores;
};
