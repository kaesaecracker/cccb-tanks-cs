import {makeApiUrl, Player} from './serverCalls.tsx';
import DataTable from "./components/DataTable.tsx";
import {useQuery} from "@tanstack/react-query";

function numberSorter(a: number, b: number) {
    return b - a;
}

export default function Scoreboard({}: {}) {
    const query = useQuery({
        queryKey: ['scores'],
        refetchInterval: 5000,
        queryFn: async () => {
            const url = makeApiUrl('/scores');
            const response = await fetch(url, {method: 'GET'});
            if (!response.ok)
                throw new Error(`response failed with code ${response.status} (${response.status})${await response.text()}`)
            return await response.json() as Player[];
        }
    });

    if (query.isError)
        return <p>{query.error.message}</p>;
    if (query.isPending)
        return <p>loading...</p>;

    return <DataTable
        data={query.data}
        columns={[
            {field: 'name'},
            {
                field: 'kills',
                visualize: p => p.scores.kills.toString(),
                sorter: (a, b) => numberSorter(a.scores.kills, b.scores.kills)
            },
            {
                field: 'deaths',
                visualize: p => p.scores.deaths.toString(),
                sorter: (a, b) => numberSorter(a.scores.deaths, b.scores.deaths)
            },
            {
                field: 'ratio',
                visualize: p => p.scores.ratio.toString(),
                sorter: (a, b) => numberSorter(a.scores.ratio, b.scores.ratio)
            },
            {
                field: 'walls',
                visualize: p => p.scores.wallsDestroyed.toString(),
                sorter: (a, b) => numberSorter(a.scores.wallsDestroyed, b.scores.wallsDestroyed)
            },
            {
                field: 'bullets',
                visualize: p => p.scores.shotsFired.toString(),
                sorter: (a, b) => numberSorter(a.scores.shotsFired, b.scores.shotsFired)
            },
            {
                field: 'powerUps',
                visualize: p => p.scores.powerUpsCollected.toString(),
                sorter: (a, b) => numberSorter(a.scores.powerUpsCollected, b.scores.powerUpsCollected)
            },
            {
                field: 'distance',
                visualize: p => p.scores.pixelsMoved.toString(),
                sorter: (a, b) => numberSorter(a.scores.pixelsMoved, b.scores.pixelsMoved)
            },
            {
                field: 'score',
                visualize: p => p.scores.overallScore.toString(),
                sorter: (a, b) => numberSorter(a.scores.overallScore, b.scores.overallScore)
            }
        ]}/>
}
