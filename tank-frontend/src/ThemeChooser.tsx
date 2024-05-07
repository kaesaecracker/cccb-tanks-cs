import Button from './components/Button.tsx';
import {getRandomTheme, HSL, hslToString, useHslTheme} from './theme.tsx';
import Dialog from './components/Dialog.tsx';
import {useState} from 'react';
import {NumberInput} from './components/Input.tsx';
import Row from './components/Row.tsx';
import Column from './components/Column.tsx';

function HslEditor({name, value, setValue}: {
    name: string;
    value: HSL;
    setValue: (value: HSL) => void
}) {
    return <Column>
        <Row>
            <div className="" style={{background: hslToString(value), border: '1px solid white', aspectRatio: '1'}}/>
            <p>{name}</p>
        </Row>
        <div style={{
            display: 'grid',
            columnGap: 'var(--padding-normal)',
            gridTemplateColumns: 'auto auto',
            gridTemplateRows: 'auto'
        }}>
            <p>Hue</p>
            <NumberInput value={Math.round(value.h)} placeholder="Hue" onChange={event => {
                setValue({...value, h: parseInt(event.target.value)});
            }}/>

            <p>Saturation</p>
            <NumberInput value={Math.round(value.s)} placeholder="Saturation" onChange={event => {
                setValue({...value, s: parseInt(event.target.value)});
            }}/>

            <p>Lightness</p>
            <NumberInput value={Math.round(value.l)} placeholder="Lightness" onChange={event => {
                setValue({...value, l: parseInt(event.target.value)});
            }}/>
        </div>
    </Column>;
}

function ThemeChooserDialog({onClose}: {
    onClose: () => void;
}) {
    const {hslTheme, setHslTheme} = useHslTheme();
    return <Dialog title="Theme editor" onClose={onClose}>
        <Button
            text="surprise me"
            onClick={() => setHslTheme(_ => getRandomTheme())}/>
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
            name="background"
            value={hslTheme.tertiary}
            setValue={value => setHslTheme(old => ({...old, tertiary: value}))}/>
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
