import React from 'react';
import {createRoot} from 'react-dom/client';
import './index.css';
import App from "./App.tsx";

function getRandomColor() {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 3; i++) {
        color += letters[Math.floor(Math.random() * letters.length)];
    }
    return color;
}

// @ts-ignore
const rootStyle = document.querySelector(':root')?.style;
if (rootStyle) {
    rootStyle.setProperty('--color-primary', getRandomColor());
    rootStyle.setProperty('--color-secondary', getRandomColor());
    rootStyle.setProperty('--color-background', getRandomColor());
}

createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <App/>
    </React.StrictMode>
);
