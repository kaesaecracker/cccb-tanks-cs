import './Input.css';

export function TextInput({onChange, className, value, placeholder, onEnter}: {
    onChange?: (value: string) => void;
    className?: string;
    value: string;
    placeholder?: string;
    onEnter?: () => void;
}) {
    return <input
        type="text"
        className={'Input ' + (className ?? '')}
        value={value}
        placeholder={placeholder}
        onChange={event => {
            if (!onChange)
                return;
            onChange(event.target.value);
        }}
        onKeyUp={event => {
            if (!onEnter || event.key !== 'Enter')
                return;
            onEnter();
        }}
    />;
}

export function NumberInput({onChange, className, value, placeholder, onEnter}: {
    onChange?: (value: number) => void;
    className?: string;
    value: number;
    placeholder?: string;
    onEnter?: () => void;
}) {
    return <input
        type="number"
        className={'Input ' + (className ?? '')}
        value={value}
        placeholder={placeholder}
        onChange={event => {
            if (!onChange)
                return;
            onChange(parseFloat(event.target.value));
        }}
        onKeyUp={event => {
            if (!onEnter || event.key !== 'Enter')
                return;
            onEnter();
        }}
    />;
}

export function RangeInput({onChange, className, value, placeholder, onEnter, min, max}: {
    onChange?: (value: number) => void;
    className?: string;
    value: number;
    min: number;
    max: number;
    placeholder?: string;
    onEnter?: () => void;
}) {
    return <input
        type="range"
        className={'Input RangeInput ' + (className ?? '')}
        value={value} min={min} max={max}
        placeholder={placeholder}
        onChange={event => {
            if (!onChange)
                return;
            onChange(parseFloat(event.target.value));
        }}
        onKeyUp={event => {
            if (!onEnter || event.key !== 'Enter')
                return;
            onEnter();
        }}
    />;
}


