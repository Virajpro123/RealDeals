import { Backdrop, Box, Button, Divider, Grid, Modal, Table, TableBody, TableCell, TableContainer, TableRow, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import LoadingComponent from "../../app/layout/LoadingComponent";
import { useAppDispatch, useAppSelector } from "../../app/store/configureStore";
import { clearBestDealDetails, fetchBestDealAsync } from "./movieCatalogSlice";
import NotFound from "../../app/errors/NotFound";


interface Props {
    isOpen: boolean;
    popupToggle: () => void;
    relatedIds: string;
}

const style = {
    position: 'absolute' as const,
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    width: '80%',
    height: '80%',
    bgcolor: 'background.paper',
    border: '2px solid #000',
    boxShadow: 24,
    p: 4,
};


export default function BestDealPopup({ isOpen, popupToggle, relatedIds }: Props) {
    const { bestDealDetails, bestDealLoadStatus} = useAppSelector(state => state.movieCatalog);
    const dispatch = useAppDispatch();
    const [open, setOpen] = useState(false);
    // const handleClose = () => setOpen(false);


    const handleClose = (_event: React.MouseEvent<HTMLButtonElement>, reason: string) => {
        setOpen(false);
        dispatch(clearBestDealDetails());
        popupToggle();
    };

    const handleCloseButton = () => {
        setOpen(false);
        popupToggle();
    };

    useEffect(() => {
        setOpen(isOpen);
    }, [isOpen]);

    useEffect(() => {
        dispatch(fetchBestDealAsync(relatedIds))
    },[dispatch, relatedIds]);

    if (bestDealLoadStatus.includes('pendingFetchBestDeal')) return <LoadingComponent message={"Loading Best Deals..."}  isBackDropInvisible= {false} />

    if (!bestDealDetails) return <NotFound />
    
    return (
        <div>
            <Modal
                open={open}
                onClose={handleClose}
                aria-labelledby="modal-modal-title"
                aria-describedby="modal-modal-description"
            >
                <Backdrop open={true} invisible={true}>

                    <Box sx={style} >

                        <Grid container spacing={6}>
                            <Grid item xs={6}>
                                <img src={bestDealDetails.movieDetails.poster} style={{ width: '100%' }} />
                            </Grid>
                            <Grid item xs={6}>
                                <Typography variant='h3'>{bestDealDetails.movieDetails.title}</Typography>
                                <Divider sx={{ mb: 2 }} />
                                <Typography variant='h4' color='secondary'>${parseFloat(bestDealDetails.movieDetails.price)} {!bestDealDetails.isRealTime && '  -  Try again after few minutes for updated deels'}</Typography>
                                <TableContainer>
                                    <Table>
                                        <TableBody sx={{ fontSize: '1.1em' }}>
                                        <TableRow>
                                                <TableCell>Provider</TableCell>
                                                <TableCell>{bestDealDetails.provider}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Year</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.year}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Plot</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.plot}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Released</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.released}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Runtime</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.runtime}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Director</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.director}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Writer</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.writer}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Actors</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.actors}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Language</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.language}</TableCell>
                                            </TableRow>
                                            <TableRow>
                                                <TableCell>Votes</TableCell>
                                                <TableCell>{bestDealDetails.movieDetails.votes}</TableCell>
                                            </TableRow>
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                            </Grid>
                        </Grid>
                        <Button onClick={handleCloseButton} style={{ left: '0%', position: 'absolute', top: '93%' }} color='success' variant="contained" size="large" fullWidth>Go Back</Button>
                    </Box>
                </Backdrop>
            </Modal>
        </div>
    )
}