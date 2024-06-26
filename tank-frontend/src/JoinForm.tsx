import {useState} from 'react';
import {useMutation} from '@tanstack/react-query';

import {makeApiUrl} from './serverCalls';
import Button from './components/Button.tsx';
import {TextInput} from './components/Input.tsx';
import Dialog from './components/Dialog.tsx';
import './JoinForm.css';

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

    return <Dialog className="JoinForm" title="Enter your name to play">
        <TextInput
            value={name}
            placeholder="player name"
            onChange={n => setName(n)}
            onEnter={confirm}
        />
        <Button
            onClick={confirm}
            disabled={disableButtons}
            text="¢ INSERT COIN"/>
        {postPlayer.isError && <p>{postPlayer.error.message}</p>}
    </Dialog>;
}
