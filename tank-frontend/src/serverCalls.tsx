export function makeApiUrl(path: string, protocol: 'http' | 'ws' = 'http') {
    return new URL(`${protocol}://${window.location.hostname}${path}`);
}

export type ServerResponse<T> = {
    ok: boolean;
    statusCode: number;
    statusText: string;
    additionalErrorText?: string;
    successResult?: T;
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

export async function fetchTyped<T>({url, method}: { url: URL; method: string; }): Promise<ServerResponse<T>> {
    const response = await fetch(url, {method});
    const result: ServerResponse<T> = {
        ok: response.ok,
        statusCode: response.status,
        statusText: response.statusText
    };

    if (response.ok)
        result.successResult = await response.json();
    else
        result.additionalErrorText = await response.text();
    return result;
}

export function postPlayer(name: string) {
    const url = makeApiUrl('/player');
    url.searchParams.set('name', name);

    return fetchTyped<string>({url, method: 'POST'});
}

export async function getMaps() {
    const url = makeApiUrl('/map');
    return await fetchTyped<string[]>({url, method: 'GET'});
}

export function postMaps(map: string) {
    const url = makeApiUrl('/map');
    url.searchParams.set('name', map);

    return fetchTyped<string>({url, method: 'POST'});
}
