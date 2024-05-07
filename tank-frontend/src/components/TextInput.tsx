import {ChangeEventHandler} from "react";
import './TextInput.css';

export default function TextInput( {onChange, className, value, placeholder, onEnter }: {
    onChange?: ChangeEventHandler<HTMLInputElement> | undefined;
    className?: string;
    value: string;
    placeholder: string;
    onEnter?: () => void;
}) {
    return <input
        type="text"
        className={'TextInput ' + (className?? '')}
        value={value}
        placeholder={placeholder}
        onChange={onChange}
        onKeyUp={event => {
            if (onEnter && event.key === 'Enter')
                onEnter();
        }}
    />;
}
