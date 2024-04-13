import {getScores, PlayerResponse} from "./serverCalls.tsx";
import {useEffect, useState} from "react";
import DataTable from "./components/DataTable.tsx";

export default function Scoreboard({}: {}) {
    const [players, setPlayers] = useState<PlayerResponse[]>([]);

    useEffect(() => {
        const refresh = () => {
            getScores().then(value => {
                if (value)
                    setPlayers(value);
            });
        };

        const timer = setInterval(refresh, 5000);
        return () => clearInterval(timer);
    }, [players]);

    return <DataTable<PlayerResponse> data={players} columns={[
        {field: 'name'},
        {field: 'kills', visualize: p => p.scores.kills},
        {field: 'deaths', visualize: p => p.scores.deaths},
        {field: 'k/d', visualize: p => p.scores.kills / p.scores.deaths}
    ]}/>
}
