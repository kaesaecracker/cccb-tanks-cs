import {useQuery} from '@tanstack/react-query'
import {Player} from './serverCalls';
import {Guid} from "./Guid.ts";
import Column from "./components/Column.tsx";

export default function PlayerInfo({playerId}: { playerId: Guid }) {
    const query = useQuery({
        queryKey: ['player'],
        refetchInterval: 1000,
        queryFn: async () => {
            const url = new URL('/player', import.meta.env.VITE_TANK_API);
            url.searchParams.set('id', playerId);

            const response = await fetch(url, {method: 'GET'});
            if (!response.ok)
                throw new Error(`response failed with code ${response.status} (${response.status})${await response.text()}`)
            return await response.json() as Player;
        }
    });

    return <Column className='PlayerInfo'>
        <h3>
            {query.isPending && 'loading...'}
            {query.isSuccess && `Playing as ${query.data.name}`}
        </h3>
        {query.isError && <p>{query.error.message}</p>}
        {query.isSuccess && <table>
            <tbody>
            <tr>
                <td>kills:</td>
                <td>{query.data?.scores?.kills ?? '?'}</td>
            </tr>
            <tr>
                <td>deaths:</td>
                <td>{query.data?.scores?.deaths ?? '?'}</td>
            </tr>
            </tbody>
        </table>}
    </Column>;
}
