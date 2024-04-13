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
    initialState: () => T
): [T, (mutator: (oldState: T) => T) => void] {
    const getInitialState = () => {
        const localStorageJson = localStorage.getItem(storageKey);
        if (localStorageJson !== null && localStorageJson !== '') {
            return JSON.parse(localStorageJson);
        }

        return initialState();
    };

    const [state, setState] = useState<T>(getInitialState);

    const setSavedState = (mut: (oldState: T) => T) => {
        localStorage.setItem(storageKey, JSON.stringify(mut(state)));
        setState(mut);
    };

    return [state, setSavedState];
}
