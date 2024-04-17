import {Player} from "./serverCalls.tsx";
import DataTable from "./components/DataTable.tsx";
import {useQuery} from "@tanstack/react-query";

function numberSorter(a: number, b: number) {
    return b - a;
}

export default function Scoreboard({}: {}) {
    const query = useQuery({
        queryKey: ['scores'],
        refetchInterval: 1000,
        queryFn: async () => {
            const url = new URL('/scores', import.meta.env.VITE_TANK_API);
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
            }
        ]}/>
}
