import {ChangeEventHandler} from "react";
import './Input.css';

export function TextInput( {onChange, className, value, placeholder, onEnter }: {
    onChange?: ChangeEventHandler<HTMLInputElement> | undefined;
    className?: string;
    value: string;
    placeholder?: string;
    onEnter?: () => void;
}) {
    return <input
        type="text"
        className={'Input ' + (className?? '')}
        value={value}
        placeholder={placeholder}
        onChange={onChange}
        onKeyUp={event => {
            if (onEnter && event.key === 'Enter')
                onEnter();
        }}
    />;
}

export function NumberInput( {onChange, className, value, placeholder, onEnter }: {
    onChange?: ChangeEventHandler<HTMLInputElement> | undefined;
    className?: string;
    value: number;
    placeholder?: string;
    onEnter?: () => void;
}) {
    return <input
        type="number"
        className={'Input ' + (className?? '')}
        value={value}
        placeholder={placeholder}
        onChange={onChange}
        onKeyUp={event => {
            if (onEnter && event.key === 'Enter')
                onEnter();
        }}
    />;
}



