import { ChangeEvent, useEffect, useState } from "react";
import { getMaps, postMaps } from "./serverCalls";

export default function MapDropDown() {
    const [mapList, setMaps] = useState<string[] | null>(null);

    useEffect(() => {
        let aborted = false;
        async function startFetch() {
            const response = await getMaps();
            if (!aborted && response.ok && response.successResult)
                setMaps(response.successResult);
        }
        startFetch();
        return () => { aborted = true };
    }, []);

    const onChange = (event: ChangeEvent<HTMLSelectElement>) => {
        postMaps(event.target.options[event.target.selectedIndex].value);
        event.preventDefault();
    };

    return <select value="maps" className='DropDown' onChange={onChange}>
        <option value="" defaultValue={""} >Choose map</option>
        {mapList?.map(m =>
            <option key={m} value={m}>{m}</option>)}
    </select>
}
