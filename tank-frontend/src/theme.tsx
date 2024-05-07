import {useStoredObjectState} from './useStoredState.ts';
import {createContext, ReactNode, useContext, useEffect, useRef, useState} from 'react';

type HSL = {
    h: number;
    s: number;
    l: number;
}

export type HslTheme = {
    primary: HSL;
    secondary: HSL;
    background: HSL;
    tertiary: HSL;
}

// @ts-ignore
const rootStyle = document.querySelector(':root')?.style;

function getRandom(min: number, max: number) {
    return min + Math.random() * (max - min);
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

function hslToString({h, s, l}: HSL) {
    return `hsl(${h},${s}%,${l}%)`;
}

function angle(a: number) {
    return ((a % 360.0) + 360) % 360;
}

export function getRandomTheme(): HslTheme {
    const goldenAngle = 180 * (3 - Math.sqrt(5));

    const background = getRandomHsl({maxSaturation: 50, minLightness: 10, maxLightness: 30});

    const otherColorParams = {
        minSaturation: background.s,
        maxSaturation: 90,
        minLightness: background.l + 20,
        maxLightness: 90
    };

    const primary = getRandomHsl(otherColorParams);
    primary.h = angle(-1 * goldenAngle + primary.h);

    const secondary = getRandomHsl(otherColorParams);
    primary.h = angle(+1 * goldenAngle + primary.h);

    const tertiary = getRandomHsl(otherColorParams);
    primary.h = angle(+3 * goldenAngle + primary.h);

    return {background, primary, secondary, tertiary};
}

function applyTheme(theme: HslTheme) {
    console.log('apply theme', theme);
    rootStyle.setProperty('--color-primary', hslToString(theme.primary));
    rootStyle.setProperty('--color-secondary', hslToString(theme.secondary));
    rootStyle.setProperty('--color-background', hslToString(theme.background));
}

type Rgba = Uint8ClampedArray;

export type RgbaTheme = {
    primary: Rgba;
    secondary: Rgba;
    background: Rgba;
    tertiary: Rgba;
}

const dummyRgbaTheme: RgbaTheme = {
    primary: new Uint8ClampedArray([0, 0, 0, 0]),
    secondary: new Uint8ClampedArray([0, 0, 0, 0]),
    background: new Uint8ClampedArray([0, 0, 0, 0]),
    tertiary: new Uint8ClampedArray([0, 0, 0, 0])
};

function hslToRgba(context: CanvasRenderingContext2D, color: HSL) {
    context.fillStyle = hslToString(color);
    context.fillRect(0, 0, 1, 1);
    return context.getImageData(0, 0, 1, 1).data;
}


const HslThemeContext = createContext<null | {
    hslTheme: HslTheme,
    setHslTheme: (mutator: (oldState: HslTheme) => HslTheme) => void
}>(null);
const RgbaThemeContext = createContext<RgbaTheme>(dummyRgbaTheme);

export function useRgbaTheme() {
    return useContext(RgbaThemeContext);
}

export function useHslTheme() {
    const context = useContext(HslThemeContext);
    if (context === null) {
        throw new Error();
    }

    return context;
}

export function ThemeProvider({children}: {
    children?: ReactNode;
}) {
    const [hslTheme, setHslTheme] = useStoredObjectState<HslTheme>('theme', getRandomTheme, {
        load: applyTheme,
        save: applyTheme
    });

    const [rgbaTheme, setRgbaTheme] = useState<RgbaTheme | null>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    useEffect(() => {
        const canvas = canvasRef.current;
        if (canvas === null)
            throw new Error('canvas null');

        const drawContext = canvas.getContext('2d', {
            alpha: true,
            colorSpace: 'srgb',
            willReadFrequently: true
        });
        if (!drawContext)
            throw new Error('could not get draw context');

        setRgbaTheme({
            background: hslToRgba(drawContext, hslTheme.background),
            primary: hslToRgba(drawContext, hslTheme.primary),
            secondary: hslToRgba(drawContext, hslTheme.secondary),
            tertiary: hslToRgba(drawContext, hslTheme.tertiary),
        });
    }, [hslTheme, canvasRef.current]);

    return <HslThemeContext.Provider value={{hslTheme, setHslTheme}}>
        <RgbaThemeContext.Provider value={rgbaTheme || dummyRgbaTheme}>
            <canvas hidden={true} ref={canvasRef}/>
            {children}
        </RgbaThemeContext.Provider>;
    </HslThemeContext.Provider>;
}
