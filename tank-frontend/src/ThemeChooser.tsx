import Button from './components/Button.tsx';
import {getRandomTheme, HSL, hslToString, useHslTheme} from './theme.tsx';
import Dialog from './components/Dialog.tsx';
import {useState} from 'react';
import {NumberInput, RangeInput} from './components/Input.tsx';
import Row from './components/Row.tsx';
import Column from './components/Column.tsx';
import './ThemeChooser.css';

function HslEditor({name, value, setValue}: {
    name: string;
    value: HSL;
    setValue: (value: HSL) => void
}) {
    const setH = (h: number) =>  setValue({...value, h});
    const setS = (s: number) =>  setValue({...value, s});
    const setL = (l: number) =>  setValue({...value, l});

    return <Column>
        <Row>
            <div className="" style={{background: hslToString(value), border: '1px solid white', aspectRatio: '1'}}/>
            <p>{name}</p>
        </Row>
        <div className='HslEditor-Inputs'>
            <p>Hue</p>
            <NumberInput value={Math.round(value.h)} onChange={setH}/>
            <RangeInput value={Math.round(value.h)} min={0} max={360} onChange={setH}/>

            <p>Saturation</p>
            <NumberInput value={Math.round(value.s)} onChange={setS}/>
            <RangeInput value={Math.round(value.s)} min={0} max={100} onChange={setS}/>

            <p>Lightness</p>
            <NumberInput value={Math.round(value.l)} onChange={setL}/>
            <RangeInput value={Math.round(value.l)} min={0} max={100} onChange={setL}/>
        </div>
    </Column>;
}

function ThemeChooserDialog({onClose}: {
    onClose: () => void;
}) {
    const {hslTheme, setHslTheme} = useHslTheme();
    return <Dialog title="Theme editor" onClose={onClose}>
        <Row>
            <Button
                text="surprise me"
                className='flex-grow'
                onClick={() => setHslTheme(_ => getRandomTheme())}/>
        </Row>
        <Column className='overflow-scroll'>
            <HslEditor
                name="background"
                value={hslTheme.background}
                setValue={value => setHslTheme(old => ({...old, background: value}))}/>
            <HslEditor
                name="primary"
                value={hslTheme.primary}
                setValue={value => setHslTheme(old => ({...old, primary: value}))}/>
            <HslEditor
                name="secondary"
                value={hslTheme.secondary}
                setValue={value => setHslTheme(old => ({...old, secondary: value}))}/>
            <HslEditor
                name="tertiary"
                value={hslTheme.tertiary}
                setValue={value => setHslTheme(old => ({...old, tertiary: value}))}/>
        </Column>
    </Dialog>;
}

export default function ThemeChooser({}: {}) {
    const [open, setOpen] = useState(false);

    return <>
        <Button
            text="☼ change colors"
            onClick={() => setOpen(true)}/>
        {open && <ThemeChooserDialog onClose={() => setOpen(false)}/>}
    </>;
}
