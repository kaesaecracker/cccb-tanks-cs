import {useState} from 'react';

export function useStoredState(storageKey: string, initialState: () => string): [string, ((newState: string) => void)] {
    const [state, setState] = useState<string>(() => localStorage.getItem(storageKey) || initialState());

    const setSavedState = (newState: string) => {
        localStorage.setItem(storageKey, newState);
        setState(newState);
    };

    return [state, setSavedState];
}

export function useStoredObjectState<T>(
    storageKey: string,
    initialState: () => T,
    options?: {
        load?: (value: T) => void;
        save?: (value: T) => void;
    }
): [T, (mutator: (oldState: T) => T) => void] {
    const getInitialState = () => {
        const localStorageJson = localStorage.getItem(storageKey);

        let result = (localStorageJson !== null && localStorageJson !== '')
            ? JSON.parse(localStorageJson) as T
            : initialState();

        if (options?.load)
            options.load(result);

        return result;
    };

    const [state, setState] = useState<T>(getInitialState);

    const setSavedState = (mut: (oldState: T) => T) => {
        setState(prevState => {
            const newState = mut(prevState);

            if (options?.save)
                options.save(newState);

            localStorage.setItem(storageKey, JSON.stringify(newState));
            return newState;
        });
    };

    return [state, setSavedState];
}
