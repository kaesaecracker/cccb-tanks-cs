import {useEffect, useState} from 'react';
import './JoinForm.css';
import {NameId, PlayerResponse, postPlayer} from './serverCalls';
import {Guid} from './Guid.ts';
import Column from "./components/Column.tsx";

export default function JoinForm({setNameId, clientId}: {
    setNameId: (mutator: (oldState: NameId) => NameId) => void,
    clientId: Guid
}) {
    const [name, setName] = useState('');
    const [clicked, setClicked] = useState(false);
    const [data, setData] = useState<PlayerResponse | null>(null);

    useEffect(() => {
        if (!clicked || data)
            return;

        try {
            postPlayer({name, id: clientId})
                .then(value => {
                    if (value)
                        setNameId(prev => ({...prev, ...value}));
                    else
                        setClicked(false);
                });
        } catch (e) {
            console.log(e);
            alert(e);
        }
    }, [clicked, setData, data, clientId, setClicked, setNameId]);

    const disableButtons = clicked || name.trim() === '';
    return <Column className='JoinForm'>
        <p className="JoinElems"> Enter your name to join the game! </p>
        <input
            className="JoinElems"
            type="text"
            value={name}
            placeholder="player name"
            onChange={e => setName(e.target.value)}
            onKeyUp={event => {
                if (event.key === 'Enter')
                    setClicked(true);
            }}
        />
        <button
            className="JoinElems"
            onClick={() => setClicked(true)}
            disabled={disableButtons}
        >
            join
        </button>
    </Column>;
}
