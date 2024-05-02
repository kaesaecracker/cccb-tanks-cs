import {ChangeEvent} from 'react';
import {makeApiUrl} from './serverCalls';
import './MapChooser.css';
import {useMutation, useQuery} from '@tanstack/react-query';

export default function MapChooser() {
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

    const onChange = (event: ChangeEvent<HTMLSelectElement>) => {
        if (event.target.selectedIndex < 1)
            return;
        event.preventDefault();

        const chosenMap = event.target.options[event.target.selectedIndex].value;
        postMap.mutate(chosenMap);
    };

    if (query.isError)
        return <></>;

    const disabled = !query.isSuccess || postMap.isPending;

    return <select className="MapChooser-DropDown" onChange={onChange} disabled={disabled}>
        <option value="" defaultValue={''}>Choose map</option>
        {query.isSuccess && query.data.map(m => <option key={m} value={m}>{m}</option>)}
    </select>;
}
