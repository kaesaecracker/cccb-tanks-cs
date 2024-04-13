export type PlayerResponse = {
    readonly name: string;
    readonly id: string;
    readonly kills: number;
    readonly deaths: number;
};

export async function fetchTyped<T>({ url, method }: { url: URL; method: string; }) {
    const response = await fetch(url, { method });
    if (!response.ok)
        return null;
    return await response.json() as T;
}

export function postPlayer(name: string) {
    const url = new URL(import.meta.env.VITE_TANK_PLAYER_URL);
    url.searchParams.set('name', name);
    return fetchTyped<PlayerResponse>({ url, method: 'POST' });
}

export function getPlayer(id: string) {
    const url = new URL(import.meta.env.VITE_TANK_PLAYER_URL);
    url.searchParams.set('id', id);
    return fetchTyped<PlayerResponse>({ url, method: 'GET' });
}
