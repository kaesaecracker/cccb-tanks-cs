import {Guid} from './Guid.ts';

export type PlayerResponse = {
    readonly name: string;
    readonly id: string;
    readonly scores: {
        readonly kills: number;
        readonly deaths: number;
    };
};

export type NameId = {
    name: string,
    id: Guid
};

export async function fetchTyped<T>({url, method}: { url: URL; method: string; }) {
    const response = await fetch(url, {method});
    if (!response.ok)
        return null;
    return await response.json() as T;
}

export function postPlayer({name, id}: NameId) {
    const url = new URL(import.meta.env.VITE_TANK_PLAYER_URL);
    url.searchParams.set('name', name);
    url.searchParams.set('id', id);

    return fetchTyped<NameId>({url, method: 'POST'});
}

export function getPlayer(id: string) {
    const url = new URL(import.meta.env.VITE_TANK_PLAYER_URL);
    url.searchParams.set('id', id);

    return fetchTyped<PlayerResponse>({url, method: 'GET'});
}

export function getScores() {
    const url = new URL('/scores', import.meta.env.VITE_TANK_API);
    return fetchTyped<PlayerResponse[]>({url, method: 'GET'});
}
