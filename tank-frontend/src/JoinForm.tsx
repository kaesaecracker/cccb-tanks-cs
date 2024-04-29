import {useState} from 'react';
import './JoinForm.css';
import {makeApiUrl} from './serverCalls';
import Column from './components/Column.tsx';
import Button from './components/Button.tsx';
import TextInput from './components/TextInput.tsx';
import {useMutation} from '@tanstack/react-query';

export default function JoinForm({onDone}: {
    onDone: (name: string) => void;
}) {
    const postPlayer = useMutation({
        retry: false,
        mutationFn: async ({name}: { name: string }) => {
            const url = makeApiUrl('/player');
            url.searchParams.set('name', name);

            const response = await fetch(url, {method: 'POST'});

            if (!response.ok)
                throw new Error(`${response.status} (${response.statusText}): ${await response.text()}`);

            onDone((await response.json()).trim());
        }
    });

    const [name, setName] = useState('');
    const disableButtons = postPlayer.isPending || name.trim() === '';

    const confirm = () => postPlayer.mutate({name});

    return <Column className="JoinForm">
        <h3> Enter your name to play </h3>
        <TextInput
            value={name}
            placeholder="player name"
            onChange={e => setName(e.target.value)}
            onEnter={confirm}
        />
        <Button
            onClick={confirm}
            disabled={disableButtons}
            text="INSERT COIN"/>
        {postPlayer.isError && <p>{postPlayer.error.message}</p>}
    </Column>;
}
