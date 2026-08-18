using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private float velocidadeRotacao = 12f;

    [Header("Pulo")]
    [SerializeField] private float forcaPulo = 7f;
    [SerializeField] private LayerMask camadaChao = ~0;
    [SerializeField] private float distanciaExtraChao = 0.15f;

    [Header("Double Jump")]
    [SerializeField] private int maxPulos = 2;
    [SerializeField] private int pulosRealizados = 0;

    [Header("Referências")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector2 inputMovimento;
    private Vector3 direcaoMovimento;

    private bool estaNoChao;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        VerificarChao();
        CalcularDirecao();
        AtualizarRotacao();
        AtualizarAnimacao();
    }

    private void FixedUpdate()
    {
        Movimentar();
    }

    // =========================
    // INPUT MOVIMENTO
    // =========================

    public void OnMove(InputAction.CallbackContext context)
    {
        inputMovimento = context.ReadValue<Vector2>();
    }

    // =========================
    // INPUT PULO
    // =========================
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (pulosRealizados >= maxPulos)
            return;

        if (estaNoChao)
        {
            if (animator != null) animator.SetTrigger("Jump");
            pulosRealizados++;
            return;
        }
        else
        {
            Pular();
            pulosRealizados++;
            if (animator != null) animator.SetTrigger("DoubleJump");
            return;
        }
    }

    // =========================
    // MOVIMENTO
    // =========================

    private void CalcularDirecao()
    {
        if (cameraTransform == null)
            return;

        Vector3 frente = cameraTransform.forward;
        Vector3 direita = cameraTransform.right;

        frente.y = 0;
        direita.y = 0;

        frente.Normalize();
        direita.Normalize();

        direcaoMovimento =
            frente * inputMovimento.y +
            direita * inputMovimento.x;

        if (direcaoMovimento.sqrMagnitude > 1f)
        {
            direcaoMovimento.Normalize();
        }
    }

    private void Movimentar()
    {
        Vector3 velocidadeAtual = rb.linearVelocity;

        Vector3 novaVelocidade =
            direcaoMovimento * velocidade;

        rb.linearVelocity = new Vector3(
            novaVelocidade.x,
            velocidadeAtual.y,
            novaVelocidade.z
        );
    }

    // =========================
    // ROTAÇÃO
    // =========================

    private void AtualizarRotacao()
    {
        if (direcaoMovimento.sqrMagnitude < 0.01f)
            return;

        Quaternion rotacaoDesejada =
            Quaternion.LookRotation(direcaoMovimento);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacaoDesejada,
            velocidadeRotacao * Time.deltaTime
        );
    }

    // =========================
    // PULO
    // =========================

    // private void Pular()
    // {
    //     Vector3 velocidadeAtual = rb.linearVelocity;

    //     rb.linearVelocity = new Vector3(
    //         velocidadeAtual.x,
    //         forcaPulo,
    //         velocidadeAtual.z
    //     );
    // }

    public void Pular()
    {
        Vector3 velocidadeAtual = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            velocidadeAtual.x,
            forcaPulo,
            velocidadeAtual.z
        );
    }

    // =========================
    // VERIFICAR CHÃO
    // =========================

    private void VerificarChao()
    {
        float distancia =
            capsule.bounds.extents.y +
            distanciaExtraChao;

        estaNoChao = Physics.Raycast(
            capsule.bounds.center,
            Vector3.down,
            distancia,
            camadaChao,
            QueryTriggerInteraction.Ignore
        );

        if (estaNoChao)
        {
            pulosRealizados = 0;
        }
    }

    // =========================
    // ANIMAÇÃO
    // =========================
    private void AtualizarAnimacao()
    {
        if (animator == null)
            return;

        bool correndo = inputMovimento != Vector2.zero;
        float run = 0;
        if(correndo) run = 1;
        animator.SetFloat("run", run);
        animator.SetBool("Grounded", estaNoChao);

        // Positivo = subindo
        // Negativo = caindo
        animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
    }
}