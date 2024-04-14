import {useEffect, useState} from 'react';
import './JoinForm.css';
import {NameId, Player, postPlayer} from './serverCalls';
import {Guid} from './Guid.ts';
import Column from "./components/Column.tsx";
import Button from "./components/Button.tsx";
import TextInput from "./components/TextInput.tsx";

export default function JoinForm({setNameId, clientId}: {
    setNameId: (mutator: (oldState: NameId) => NameId) => void,
    clientId: Guid
}) {
    const [clicked, setClicked] = useState(false);
    const [data, setData] = useState<Player | null>(null);
    const [errorText, setErrorText] = useState<string | null>();

    useEffect(() => {
        if (!clicked || data)
            return;

        postPlayer({name, id: clientId})
            .then(response => {
                if (response.ok && response.successResult) {
                    setNameId(_ => response.successResult!);
                    setErrorText(null);
                    return;
                }

                if (response.additionalErrorText)
                    setErrorText(`${response.statusCode} (${response.statusText}): ${response.additionalErrorText}`);
                else
                    setErrorText(`${response.statusCode} (${response.statusText})`);

                setClicked(false);
            });
    }, [clicked, setData, data, clientId, setClicked, setNameId, errorText]);

    const [name, setName] = useState('');
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
        {errorText && <p>{errorText}</p>}
    </Column>;
}
