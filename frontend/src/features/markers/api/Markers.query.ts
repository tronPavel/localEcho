import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {createMarker, getMarkers} from "../api/Markers.api.ts";

export const useMarkers = () => useQuery({
    queryKey: ['markers'],
    queryFn: getMarkers,
})

export const useCreateMarker = () => {
 const queryClient = useQueryClient();
 return useMutation({
     mutationFn: createMarker,
     onSuccess: ()=> queryClient.invalidateQueries({queryKey: ['markers']}),
 })
}
