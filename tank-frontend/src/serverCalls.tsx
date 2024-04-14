import {Guid} from './Guid.ts';

export type ServerResponse<T> = {
    ok: boolean;
    statusCode: number;
    statusText: string;
    additionalErrorText?: string;
    successResult?: T;
}

export type Player = {
    readonly name: string;
    readonly id: Guid;
    readonly scores: {
        readonly kills: number;
        readonly deaths: number;
    };
};

export type NameId = {
    name: string,
    id: Guid
};

export async function fetchTyped<T>({url, method}: { url: URL; method: string; }): Promise<ServerResponse<T>> {
    const response = await fetch(url, {method});
    const result: ServerResponse<T> = {
        ok: response.ok,
        statusCode: response.status,
        statusText: response.statusText
    }

    if (response.ok)
        result.successResult = await response.json();
    else
        result.additionalErrorText = await response.text();
    return result;
}

export function postPlayer({name, id}: NameId) {
    const url = new URL(import.meta.env.VITE_TANK_PLAYER_URL);
    url.searchParams.set('name', name);
    url.searchParams.set('id', id);

    return fetchTyped<NameId>({url, method: 'POST'});
}

export function getPlayer(id: Guid) {
    const url = new URL(import.meta.env.VITE_TANK_PLAYER_URL);
    url.searchParams.set('id', id);

    return fetchTyped<Player>({url, method: 'GET'});
}

export function getScores() {
    const url = new URL('/scores', import.meta.env.VITE_TANK_API);
    return fetchTyped<Player[]>({url, method: 'GET'});
}
