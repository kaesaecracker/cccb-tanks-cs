import Button from './components/Button.tsx';
import {getRandomTheme, useHslTheme} from './theme.tsx';

export default function ThemeChooser({}: {}) {
    const {setHslTheme} = useHslTheme();
    return <>
        <Button
            text="☼ change colors"
            onClick={() => setHslTheme(_ => getRandomTheme())}/>
    </>;
}
