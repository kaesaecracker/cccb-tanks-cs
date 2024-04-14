import {useCallback, useState} from 'react';
import ClientScreen from './ClientScreen';
import Controls from './Controls.tsx';
import JoinForm from './JoinForm.tsx';
import PlayerInfo from './PlayerInfo.tsx';
import {useStoredObjectState} from './useStoredState.ts';
import {NameId, postPlayer} from './serverCalls.tsx';
import Column from "./components/Column.tsx";
import Row from "./components/Row.tsx";
import Scoreboard from "./Scoreboard.tsx";
import Button from "./components/Button.tsx";
import './App.css';
import {getRandomTheme, useStoredTheme} from "./theme.ts";

const getNewNameId = () => ({
    id: crypto.randomUUID(),
    name: ''
});

export default function App() {
    const [theme, setTheme] = useStoredTheme();
    const [nameId, setNameId] = useStoredObjectState<NameId>('access', getNewNameId);

    const [isLoggedIn, setLoggedIn] = useState<boolean>(false);
    const logout = () => setLoggedIn(false);

    useCallback(async () => {
        if (isLoggedIn)
            return;
        const result = await postPlayer(nameId);
        setLoggedIn(result !== null);
    }, [nameId, isLoggedIn])();

    return <Column className='flex-grow'>
        <Row>
            <h1 className='flex-grow'>CCCB-Tanks!</h1>
            <Button text='change colors' onClick={() => setTheme(getRandomTheme())} />
            <Button
                onClick={() => window.open('https://github.com/kaesaecracker/cccb-tanks-cs', '_blank')?.focus()}
                text='GitHub'/>
            {nameId.name !== '' &&
                <Button onClick={() => setNameId(getNewNameId)} text='logout'/>}
        </Row>
        <ClientScreen logout={logout} theme={theme}/>
        {nameId.name === '' && <JoinForm setNameId={setNameId} clientId={nameId.id}/>}
        {isLoggedIn && <Row className='GadgetRows'>
            <Controls playerId={nameId.id} logout={logout}/>
            <PlayerInfo playerId={nameId.id} logout={logout}/>
            <Scoreboard/>
        </Row>
        }
    </Column>;
}
