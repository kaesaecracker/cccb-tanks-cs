import {ChangeEvent} from 'react';
import {makeApiUrl} from './serverCalls';
import './MapChooser.css';
import {useQuery} from '@tanstack/react-query';

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

    const onChange = (event: ChangeEvent<HTMLSelectElement>) => {
        if (event.target.selectedIndex < 1)
            return;
        event.preventDefault();

        const url = makeApiUrl('/map');
        url.searchParams.set('name', event.target.options[event.target.selectedIndex].value);

        fetch(url, {method: 'POST'});
    };

    return <select value="maps" className="MapChooser-DropDown" onChange={onChange}>
        <option value="" defaultValue={''}>Choose map</option>
        {query.isSuccess && query.data.map(m =>
            <option key={m} value={m}>{m}</option>)}
    </select>;
}
