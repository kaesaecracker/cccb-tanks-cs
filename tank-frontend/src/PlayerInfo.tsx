import {useQuery} from '@tanstack/react-query'
import {makeApiUrl, Player} from './serverCalls';
import {Guid} from "./Guid.ts";
import Column from "./components/Column.tsx";

function ScoreRow({name, value}: {
    name: string;
    value?: any;
}) {
    return <tr>
        <td>{name}</td>
        <td>{value ?? '?'}</td>
    </tr>;
}

export default function PlayerInfo({playerId}: { playerId: Guid }) {
    const query = useQuery({
        queryKey: ['player'],
        refetchInterval: 1000,
        queryFn: async () => {
            const url = makeApiUrl('/player');
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
            <ScoreRow name='kills' value={query.data?.scores?.kills}/>
            <ScoreRow name='deaths' value={query.data?.scores?.deaths}/>
            <ScoreRow name='walls' value={query.data?.scores?.wallsDestroyed}/>
            </tbody>
        </table>}
    </Column>;
}
