import Spinner from "react-spinner-material";

function LoadingComponent() {
    return (
        <div style={{display: 'flex',  justifyContent:'center', alignItems:'center', height: '100vh'}}>
            <Spinner radius={60} color={"#e10600"} stroke={2} visible={true}/>
        </div>
    );
}

export default LoadingComponent;