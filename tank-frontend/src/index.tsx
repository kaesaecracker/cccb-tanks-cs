import React from 'react';
import {createRoot} from 'react-dom/client';
import './index.css';
import App from "./App.tsx";
import {setRandomTheme} from "./theme.ts";


setRandomTheme();

createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <App/>
    </React.StrictMode>
);
