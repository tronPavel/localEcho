import {RoutedModal} from "@/shared/ui/Modal/RoutedModal.tsx";
import {useParams} from "react-router-dom";
import {MarkerDetailsFeature} from "@/features/view-marker/ui/MarkerDetailsFeature.tsx";

export const MarkerDetailsPage = () => {
    const { id } = useParams();
    return (
        <RoutedModal>
            {id && <MarkerDetailsFeature id={id} />}
        </RoutedModal>
    );
}