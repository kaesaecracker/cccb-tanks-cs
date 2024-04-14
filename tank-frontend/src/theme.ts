function getRandomColor() {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 3; i++) {
        color += letters[Math.floor(Math.random() * letters.length)];
    }
    return color;
}


export type Theme = {
    primary?: string;
    secondary?: string;
    background?: string;
}

// @ts-ignore
const rootStyle = document.querySelector(':root')?.style;

export function setTheme(theme: Theme) {
    if (!rootStyle)
        return;
    rootStyle.setProperty('--color-primary', theme.primary);
    rootStyle.setProperty('--color-secondary', theme.secondary);
    rootStyle.setProperty('--color-background', theme.background);
}

export function getTheme(): Theme {
    if (!rootStyle)
        return {};
    return {
        primary: rootStyle.getPropertyValue('--color-primary'),
        secondary: rootStyle.getPropertyValue('--color-secondary'),
        background: rootStyle.getPropertyValue('--color-background')
    };
}

export function setRandomTheme() {
    setTheme({
        primary: getRandomColor(),
        secondary: getRandomColor(),
        background: getRandomColor()
    })
}
