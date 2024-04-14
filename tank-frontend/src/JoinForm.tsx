import {useEffect, useState} from 'react';
import './JoinForm.css';
import {NameId, PlayerResponse, postPlayer} from './serverCalls';
import {Guid} from './Guid.ts';
import Column from "./components/Column.tsx";
import Button from "./components/Button.tsx";
import TextInput from "./components/TextInput.tsx";

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
    const setClickedTrue = () => setClicked(true);

    return <Column className='JoinForm'>
        <h3> Enter your name to play </h3>
        <TextInput
            value={name}
            placeholder="player name"
            onChange={e => setName(e.target.value)}
            onEnter={setClickedTrue}
        />
        <Button
            onClick={setClickedTrue}
            disabled={disableButtons}
            text='INSERT COIN'/>
    </Column>;
}
