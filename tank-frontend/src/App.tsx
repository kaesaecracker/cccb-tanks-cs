import {useState} from 'react';

import ClientScreen from './ClientScreen';
import Controls from './Controls.tsx';
import JoinForm from './JoinForm.tsx';
import PlayerInfo from './PlayerInfo.tsx';
import Column from './components/Column.tsx';
import Row from './components/Row.tsx';
import Scoreboard from './Scoreboard.tsx';
import Button from './components/Button.tsx';
import MapChooser from './MapChooser.tsx';
import {ThemeProvider} from './theme.tsx';
import './App.css';
import ThemeChooser from './ThemeChooser.tsx';

export default function App() {
    const [name, setName] = useState<string | null>(null);

    return <ThemeProvider>
        <Column className="flex-grow">

            <ClientScreen player={name}/>

            <Row>
                <h1 className="flex-grow">CCCB-Tanks!</h1>
                <MapChooser/>
                <ThemeChooser/>
                <Button
                    onClick={() => window.open('https://github.com/kaesaecracker/cccb-tanks-cs', '_blank')?.focus()}
                    text="⌂ source"/>
                {name !== '' &&
                    <Button onClick={() => setName(_ => '')} text="∩ logout"/>}
            </Row>

            {name || <JoinForm onDone={name => setName(_ => name)}/>}

            <Row className="GadgetRows">
                {name && <Controls player={name}/>}
                {name && <PlayerInfo player={name}/>}
                <Scoreboard/>
            </Row>

        </Column>
    </ThemeProvider>;
}
