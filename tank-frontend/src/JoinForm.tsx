import {useEffect, useState} from 'react';
import './JoinForm.css';
import {makeApiUrl, Player} from './serverCalls';
import Column from './components/Column.tsx';
import Button from './components/Button.tsx';
import TextInput from './components/TextInput.tsx';

export default function JoinForm({onDone}: {
    onDone: (name: string) => void;
}) {
    const [clicked, setClicked] = useState(false);
    const [data, setData] = useState<Player | null>(null);
    const [errorText, setErrorText] = useState<string | null>();

    useEffect(() => {
        if (!clicked || data)
            return;

        const url = makeApiUrl('/player');
        url.searchParams.set('name', name);

        fetch(url, {method: 'POST'})
            .then(async response => {
                if (!response.ok) {
                    setErrorText(`${response.status} (${response.statusText}): ${await response.text()}`);
                    setClicked(false);
                    return;
                }

                onDone((await response.json()).trim());
                setErrorText(null);
            });
    }, [clicked, setData, data, setClicked, onDone, errorText]);

    const [name, setName] = useState('');
    const disableButtons = clicked || name.trim() === '';
    const setClickedTrue = () => setClicked(true);

    return <Column className="JoinForm">
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
            text="INSERT COIN"/>
        {errorText && <p>{errorText}</p>}
    </Column>;
}
