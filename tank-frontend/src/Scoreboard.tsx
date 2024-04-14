import {getScores, Player} from "./serverCalls.tsx";
import {useEffect, useState} from "react";
import DataTable from "./components/DataTable.tsx";

function numberSorter(a: number, b: number) {
    return b - a;
}

export default function Scoreboard({}: {}) {
    const [players, setPlayers] = useState<Player[]>([]);

    useEffect(() => {
        const refresh = () => {
            getScores().then(value => {
                if (value.successResult)
                    setPlayers(value.successResult);
            });
        };

        const timer = setInterval(refresh, 5000);
        return () => clearInterval(timer);
    }, [players]);

    return <DataTable
        data={players}
        className='flex-grow'
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
