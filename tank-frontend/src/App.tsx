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

const getNewNameId = () => ({
    id: crypto.randomUUID(),
    name: ''
});

export default function App() {
    const [nameId, setNameId] = useStoredObjectState<NameId>('access', getNewNameId);

    const [isLoggedIn, setLoggedIn] = useState<boolean>(false);
    const logout = () => setLoggedIn(false);

    useCallback(async () => {
        if (isLoggedIn)
            return;
        const result = await postPlayer(nameId);
        setLoggedIn(result !== null);
    }, [nameId, isLoggedIn])();

    return <Column className='grow'>
        <Row>
            <h1 className='grow'>Tanks!</h1>
            {nameId.name !== '' &&
                <Button className='PlayerInfo-Reset' onClick={() => setNameId(getNewNameId)} text='logout'/>}
        </Row>
        <ClientScreen logout={logout}/>
        {nameId.name === '' && <JoinForm setNameId={setNameId} clientId={nameId.id}/>}
        {isLoggedIn && <Row className='GadgetRows'>
            <Controls playerId={nameId.id} logout={logout}/>
            <PlayerInfo playerId={nameId.id} logout={logout}/>
            <Scoreboard/>
        </Row>
        }
    </Column>;
}
