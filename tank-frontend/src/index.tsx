import React, {useState} from 'react';
import './index.css';
import ClientScreen from './ClientScreen';
import Controls from './Controls.tsx';
import JoinForm from './JoinForm.tsx';
import {createRoot} from 'react-dom/client';

function App() {
    const [id, setId] = useState<string | null>(null);

    return <>
        {id === null && <JoinForm onDone={name => setId(name)}/>}
        <ClientScreen/>
        {id == null || <Controls playerId={id}/>} 
    </>;
}

createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <App/>
    </React.StrictMode>
);
