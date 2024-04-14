import {ChangeEventHandler} from "react";
import './TextInput.css';

export default function TextInput(props: {
    onChange?: ChangeEventHandler<HTMLInputElement> | undefined;
    className?: string;
    value: string;
    placeholder: string;
    onEnter?: () => void;
}) {
    return <input
        {...props}
        type="text"
        className={'TextInput ' + (props.className?? '')}

        onKeyUp={event => {
            if (props.onEnter && event.key === 'Enter')
                props.onEnter();
        }}
    />;
}
