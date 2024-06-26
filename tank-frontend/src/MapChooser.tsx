import {useState} from 'react';
import {useMutation, useQuery} from '@tanstack/react-query';
import {makeApiUrl, MapInfo} from './serverCalls';
import Dialog from './components/Dialog.tsx';
import PixelGridCanvas from './components/PixelGridCanvas.tsx';
import Column from './components/Column.tsx';
import Button from './components/Button.tsx';
import Row from './components/Row.tsx';
import './MapChooser.css';
import Spacer from './components/Spacer.tsx';

function base64ToArrayBuffer(base64: string) {
    const binaryString = atob(base64);
    const bytes = new Uint8ClampedArray(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes;
}

function MapPreview({mapName, highlight, onClick}: {
    readonly mapName: string,
    readonly highlight: boolean,
    readonly onClick: () => void
}) {
    const query = useQuery({
        queryKey: ['get-map', mapName],
        queryFn: async () => {
            const url = makeApiUrl(`/map/${mapName}`);
            const response = await fetch(url, {method: 'GET'});
            if (!response.ok)
                throw new Error(`response failed with code ${response.status} (${response.status})${await response.text()}`);
            return await response.json() as MapInfo;
        }
    });

    if (query.isError)
        return <p>{query.error.message}</p>;
    else if (query.isPending)
        return <p>loading...</p>;

    const {name, preview} = query.data;

    return <Column
        key={mapName}
        className={'MapChooser-Preview' + (highlight ? ' MapChooser-Preview-Highlight' : '')}
        onClick={onClick}
    >
        <PixelGridCanvas pixels={base64ToArrayBuffer(preview)}/>
        <p>{name}</p>
    </Column>;
}


function MapChooserDialog({mapNames, onClose, onConfirm}: {
    readonly mapNames: string[];
    readonly onConfirm: (mapName: string) => void;
    readonly onClose: () => void;
}) {
    const [chosenMap, setChosenMap] = useState<string>();
    return <Dialog
        className='MapChooser-Dialog'
        title="Choose a map"
        onClose={onClose}
    >
        <Row className="MapChooser-Row overflow-scroll">
            {mapNames.map(name => <MapPreview
                key={name}
                mapName={name}
                highlight={chosenMap == name}
                onClick={() => setChosenMap(name)}
            />)}
        </Row>
        <Row>
            <Spacer/>
            <Button text="« cancel" onClick={onClose}/>
            <Button text="√ confirm" disabled={!chosenMap} onClick={() => chosenMap && onConfirm(chosenMap)}/>
        </Row>
    </Dialog>;
}

export default function MapChooser({}: {}) {
    const query = useQuery({
        queryKey: ['get-maps'],
        queryFn: async () => {
            const url = makeApiUrl('/map');
            const response = await fetch(url, {method: 'GET'});
            if (!response.ok)
                throw new Error(`response failed with code ${response.status} (${response.status})${await response.text()}`);
            return await response.json() as string[];
        }
    });

    const postMap = useMutation({
        retry: false,
        mutationFn: async (map: string) => {
            const url = makeApiUrl('/map');
            url.searchParams.set('name', map);

            const response = await fetch(url, {method: 'POST'});
            if (!response.ok)
                throw new Error(`${response.status} (${response.statusText}): ${await response.text()}`);
        }
    });

    const [open, setOpen] = useState(false);

    return <>
        <Button text="▓ Change map"
                disabled={!query.isSuccess || postMap.isPending}
                onClick={() => setOpen(true)}/>
        {query.isSuccess && open &&
            <MapChooserDialog
                mapNames={query.data!}
                onClose={() => setOpen(false)}
                onConfirm={name => {
                    setOpen(false);
                    postMap.mutate(name);
                }}
            />
        }
    </>;
}
