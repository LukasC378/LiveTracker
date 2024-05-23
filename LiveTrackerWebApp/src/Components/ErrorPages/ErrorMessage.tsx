const ErrorMessageComponent = (props: { message: string }) => {
    return(
        <div className={"container-fluid"}>
            <h1 style={{display: 'flex',  justifyContent:'center', alignItems:'center', height: '50vh'}}>{props.message}</h1>
        </div>
    )
}

export default ErrorMessageComponent;