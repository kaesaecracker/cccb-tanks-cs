import React, {useCallback, useState} from 'react';
import './index.css';
import ClientScreen from './ClientScreen';
import Controls from './Controls.tsx';
import JoinForm from './JoinForm.tsx';
import {createRoot} from 'react-dom/client';
import PlayerInfo from './PlayerInfo.tsx';
import {useStoredObjectState} from './useStoredState.ts';
import {NameId, postPlayer} from './serverCalls.tsx';

function App() {
    const [nameId, setNameId] = useStoredObjectState<NameId>('access', () => ({
        id: crypto.randomUUID(),
        name: ''
    }));

    const [isLoggedIn, setLoggedIn] = useState<boolean>(false);
    const logout = () => setLoggedIn(false);

    useCallback(async () => {
        if (isLoggedIn)
            return;
        const result = await postPlayer(nameId);
        setLoggedIn(result !== null);
    }, [nameId, isLoggedIn])();

    return <>
        {nameId.name === '' && <JoinForm setNameId={setNameId} clientId={nameId.id}/>}
        {isLoggedIn && <PlayerInfo playerId={nameId.id} logout={logout}/>}
        <ClientScreen logout={logout}/>
        {isLoggedIn && <Controls playerId={nameId.id} logout={logout}/>}
    </>;
}

createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <App/>
    </React.StrictMode>
);
