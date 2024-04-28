import ClientScreen from './ClientScreen';
import Controls from './Controls.tsx';
import JoinForm from './JoinForm.tsx';
import PlayerInfo from './PlayerInfo.tsx';
import Column from './components/Column.tsx';
import Row from './components/Row.tsx';
import Scoreboard from './Scoreboard.tsx';
import Button from './components/Button.tsx';
import './App.css';
import {getRandomTheme, useStoredTheme} from './theme.ts';
import {useState} from 'react';

export default function App() {
    const [theme, setTheme] = useStoredTheme();
    const [name, setName] = useState<string | null>(null);

    return <Column className="flex-grow">

        <ClientScreen theme={theme} player={name}/>

        <Row>
            <h1 className="flex-grow">CCCB-Tanks!</h1>
            <Button text="change colors" onClick={() => setTheme(_ => getRandomTheme())}/>
            <Button
                onClick={() => window.open('https://github.com/kaesaecracker/cccb-tanks-cs', '_blank')?.focus()}
                text="GitHub"/>
            {name !== '' &&
                <Button onClick={() => setName(_ => '')} text="logout"/>}
        </Row>

        {name || <JoinForm onDone={name => setName(_ => name)}/>}

        <Row className="GadgetRows">
            {name && <Controls player={name}/>}
            {name && <PlayerInfo player={name}/>}
            <Scoreboard/>
        </Row>

    </Column>;
}
