import {useStoredObjectState} from "./useStoredState.ts";

export type Theme = {
    primary: HSL;
    secondary: HSL;
    background: HSL;
}

// @ts-ignore
const rootStyle = document.querySelector(':root')?.style;

function getRandom(min: number, max: number) {
    return min + Math.random() * (max - min);
}

type HSL = {
    h: number;
    s: number;
    l: number;
}

function getRandomHsl(params: {
    minHue?: number,
    maxHue?: number,
    minSaturation?: number,
    maxSaturation?: number,
    minLightness?: number,
    maxLightness?: number,
}): HSL {
    const values = {
        minHue: 0,
        maxHue: 360,
        minSaturation: 0,
        maxSaturation: 100,
        minLightness: 0,
        maxLightness: 100,
        ...params
    };
    const h = getRandom(values.minHue, values.maxHue);
    const s = getRandom(values.minSaturation, values.maxSaturation);
    const l = getRandom(values.minLightness, values.maxLightness);
    return {h, s, l};
}

export function hslToString({h, s, l}: HSL) {
    return `hsl(${h},${s}%,${l}%)`;
}

function angle(a: number) {
    return ((a % 360.0) + 360) % 360;
}

const goldenAngle = 180 * (3 - Math.sqrt(5));


export function getRandomTheme(): Theme {
    const background = getRandomHsl({maxSaturation: 50, minLightness: 10, maxLightness: 40});

    const primary = getRandomHsl({minSaturation: background.s * 1.2, minLightness: background.l + 20});
    primary.h = angle(-Math.floor(1 + Math.random() * 2) * goldenAngle + primary.h);

    const secondary = getRandomHsl({minSaturation: background.s * 1.2, minLightness: background.l + 20});
    primary.h = angle(+Math.floor(1 + Math.random() * 2) * goldenAngle + primary.h);

    return {background, primary, secondary};
}

export function useStoredTheme(): [Theme, (theme: Theme) => void] {
    const [theme, setSavedTheme] = useStoredObjectState<Theme>('theme', getRandomTheme);

    function setTheme(theme: Theme) {
        console.log('set theme', theme);
        rootStyle.setProperty('--color-primary', hslToString(theme.primary));
        rootStyle.setProperty('--color-secondary', hslToString(theme.secondary));
        rootStyle.setProperty('--color-background', hslToString(theme.background));
        setSavedTheme(_ => theme);
    }

    return [theme, setTheme];
}
